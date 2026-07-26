using Lamour.Application.Features.Backups.Dtos;
using Lamour.Application.Features.Backups.Repositories;
using Lamour.Domain.Exceptions;

namespace Lamour.Application.Features.Backups.UseCases;

public class UpdateBackupScheduleUseCase : IUpdateBackupScheduleUseCase
{
    private readonly IBackupScheduleRepository _repo;
    public UpdateBackupScheduleUseCase(IBackupScheduleRepository repo) => _repo = repo;

    public async Task<BackupScheduleResponseDto> ExecuteAsync(UpdateBackupScheduleRequestDto request, CancellationToken ct = default)
    {
        if (request.RetentionDays <= 0)
            throw new DomainException("Số ngày giữ bản sao lưu phải lớn hơn 0.");

        if (request.IntervalDays <= 0)
            throw new DomainException("Số ngày giữa 2 lần chạy backup phải lớn hơn 0.");

        if (!TimeOnly.TryParse(request.TimeOfDay, out var timeOfDay))
            throw new DomainException("Giờ chạy backup không hợp lệ, định dạng phải là HH:mm.");

        if (string.IsNullOrWhiteSpace(request.Directory))
            throw new DomainException("Thư mục lưu trữ không được để trống.");

        var schedule = await _repo.GetAsync(ct);
        schedule.IsEnabled     = request.IsEnabled;
        schedule.TimeOfDay     = timeOfDay;
        schedule.IntervalDays  = request.IntervalDays;
        schedule.RetentionDays = request.RetentionDays;
        schedule.Directory     = request.Directory.Trim();

        var updated = await _repo.UpdateAsync(schedule, ct);
        return GetBackupScheduleUseCase.MapToDto(updated);
    }
}
