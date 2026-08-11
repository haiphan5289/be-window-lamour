using Lamour.Api.Hubs;
using Lamour.Api.Middleware;
using Lamour.Api.Realtime;
using Lamour.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "supersecretkey_changeme_32chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer           = false,
            ValidateAudience         = false,
        };
        // SignalR WebSocket connections can't set an Authorization header, so the
        // WPF client passes the JWT via ?access_token= query string instead.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── Suppliers DI ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.Repositories.ISupplierRepository,
                           Lamour.Infrastructure.Repositories.SupplierRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.UseCases.IGetSuppliersUseCase,
                           Lamour.Application.Features.Suppliers.UseCases.GetSuppliersUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.UseCases.ICreateSupplierUseCase,
                           Lamour.Application.Features.Suppliers.UseCases.CreateSupplierUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.UseCases.IUpdateSupplierUseCase,
                           Lamour.Application.Features.Suppliers.UseCases.UpdateSupplierUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.UseCases.IDeleteSupplierUseCase,
                           Lamour.Application.Features.Suppliers.UseCases.DeleteSupplierUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Suppliers.UseCases.IDuplicateSupplierUseCase,
                           Lamour.Application.Features.Suppliers.UseCases.DuplicateSupplierUseCase>();

// ── Products DI ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Products.Repositories.IProductRepository,
                           Lamour.Infrastructure.Repositories.ProductRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Products.UseCases.IGetProductsUseCase,
                           Lamour.Application.Features.Products.UseCases.GetProductsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Products.UseCases.ICreateProductUseCase,
                           Lamour.Application.Features.Products.UseCases.CreateProductUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Products.UseCases.IUpdateProductUseCase,
                           Lamour.Application.Features.Products.UseCases.UpdateProductUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Products.UseCases.IDeleteProductUseCase,
                           Lamour.Application.Features.Products.UseCases.DeleteProductUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Products.UseCases.IDuplicateProductUseCase,
                           Lamour.Application.Features.Products.UseCases.DuplicateProductUseCase>();

// ── Categories DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Categories.Repositories.ICategoryRepository,
                           Lamour.Infrastructure.Repositories.CategoryRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Categories.UseCases.IGetCategoriesUseCase,
                           Lamour.Application.Features.Categories.UseCases.GetCategoriesUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Categories.UseCases.ICreateCategoryUseCase,
                           Lamour.Application.Features.Categories.UseCases.CreateCategoryUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Categories.UseCases.IUpdateCategoryUseCase,
                           Lamour.Application.Features.Categories.UseCases.UpdateCategoryUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Categories.UseCases.IDeleteCategoryUseCase,
                           Lamour.Application.Features.Categories.UseCases.DeleteCategoryUseCase>();

// ── Product Units DI ─────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.ProductUnits.Repositories.IProductUnitRepository,
                           Lamour.Infrastructure.Repositories.ProductUnitRepository>();
builder.Services.AddScoped<Lamour.Application.Features.ProductUnits.UseCases.IGetProductUnitsUseCase,
                           Lamour.Application.Features.ProductUnits.UseCases.GetProductUnitsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ProductUnits.UseCases.ICreateProductUnitUseCase,
                           Lamour.Application.Features.ProductUnits.UseCases.CreateProductUnitUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ProductUnits.UseCases.IUpdateProductUnitUseCase,
                           Lamour.Application.Features.ProductUnits.UseCases.UpdateProductUnitUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ProductUnits.UseCases.IDeleteProductUnitUseCase,
                           Lamour.Application.Features.ProductUnits.UseCases.DeleteProductUnitUseCase>();

// ── Account Settings DI ──────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.AccountSettings.Repositories.IAccountSettingRepository,
                           Lamour.Infrastructure.Repositories.AccountSettingRepository>();
builder.Services.AddScoped<Lamour.Application.Features.AccountSettings.UseCases.IGetAccountSettingsUseCase,
                           Lamour.Application.Features.AccountSettings.UseCases.GetAccountSettingsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.AccountSettings.UseCases.ICreateAccountSettingUseCase,
                           Lamour.Application.Features.AccountSettings.UseCases.CreateAccountSettingUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.AccountSettings.UseCases.IUpdateAccountSettingUseCase,
                           Lamour.Application.Features.AccountSettings.UseCases.UpdateAccountSettingUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.AccountSettings.UseCases.IDeleteAccountSettingUseCase,
                           Lamour.Application.Features.AccountSettings.UseCases.DeleteAccountSettingUseCase>();

// ── Departments DI ───────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Departments.Repositories.IDepartmentRepository,
                           Lamour.Infrastructure.Repositories.DepartmentRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Departments.UseCases.IGetDepartmentsUseCase,
                           Lamour.Application.Features.Departments.UseCases.GetDepartmentsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Departments.UseCases.ICreateDepartmentUseCase,
                           Lamour.Application.Features.Departments.UseCases.CreateDepartmentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Departments.UseCases.IUpdateDepartmentUseCase,
                           Lamour.Application.Features.Departments.UseCases.UpdateDepartmentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Departments.UseCases.IDeleteDepartmentUseCase,
                           Lamour.Application.Features.Departments.UseCases.DeleteDepartmentUseCase>();

// ── Expense Categories DI ────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.ExpenseCategories.Repositories.IExpenseCategoryRepository,
                           Lamour.Infrastructure.Repositories.ExpenseCategoryRepository>();
builder.Services.AddScoped<Lamour.Application.Features.ExpenseCategories.UseCases.IGetExpenseCategoriesUseCase,
                           Lamour.Application.Features.ExpenseCategories.UseCases.GetExpenseCategoriesUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ExpenseCategories.UseCases.ICreateExpenseCategoryUseCase,
                           Lamour.Application.Features.ExpenseCategories.UseCases.CreateExpenseCategoryUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ExpenseCategories.UseCases.IUpdateExpenseCategoryUseCase,
                           Lamour.Application.Features.ExpenseCategories.UseCases.UpdateExpenseCategoryUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.ExpenseCategories.UseCases.IDeleteExpenseCategoryUseCase,
                           Lamour.Application.Features.ExpenseCategories.UseCases.DeleteExpenseCategoryUseCase>();

// ── Warehouses DI ────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Warehouses.Repositories.IWarehouseRepository,
                           Lamour.Infrastructure.Repositories.WarehouseRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouses.UseCases.IGetWarehousesUseCase,
                           Lamour.Application.Features.Warehouses.UseCases.GetWarehousesUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouses.UseCases.ICreateWarehouseUseCase,
                           Lamour.Application.Features.Warehouses.UseCases.CreateWarehouseUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouses.UseCases.IUpdateWarehouseUseCase,
                           Lamour.Application.Features.Warehouses.UseCases.UpdateWarehouseUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouses.UseCases.IDeleteWarehouseUseCase,
                           Lamour.Application.Features.Warehouses.UseCases.DeleteWarehouseUseCase>();

// ── Customers DI ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Customers.Repositories.ICustomerRepository,
                           Lamour.Infrastructure.Repositories.CustomerRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IGetCustomersUseCase,
                           Lamour.Application.Features.Customers.UseCases.GetCustomersUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IGetNextCustomerCodeUseCase,
                           Lamour.Application.Features.Customers.UseCases.GetNextCustomerCodeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.ICreateCustomerUseCase,
                           Lamour.Application.Features.Customers.UseCases.CreateCustomerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IUpdateCustomerUseCase,
                           Lamour.Application.Features.Customers.UseCases.UpdateCustomerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IDeleteCustomerUseCase,
                           Lamour.Application.Features.Customers.UseCases.DeleteCustomerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IDuplicateCustomerUseCase,
                           Lamour.Application.Features.Customers.UseCases.DuplicateCustomerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Customers.UseCases.IImportExcelCustomersUseCase,
                           Lamour.Infrastructure.UseCases.ImportExcelCustomersUseCase>();

// ── Employees DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Employees.Repositories.IEmployeeRepository,
                           Lamour.Infrastructure.Repositories.EmployeeRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Employees.UseCases.IGetEmployeesUseCase,
                           Lamour.Application.Features.Employees.UseCases.GetEmployeesUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Employees.UseCases.ICreateEmployeeUseCase,
                           Lamour.Application.Features.Employees.UseCases.CreateEmployeeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Employees.UseCases.IUpdateEmployeeUseCase,
                           Lamour.Application.Features.Employees.UseCases.UpdateEmployeeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Employees.UseCases.IDeleteEmployeeUseCase,
                           Lamour.Application.Features.Employees.UseCases.DeleteEmployeeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Employees.UseCases.IDuplicateEmployeeUseCase,
                           Lamour.Application.Features.Employees.UseCases.DuplicateEmployeeUseCase>();

// ── Warehouse DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Warehouse.Repositories.IInventoryRepository,
                           Lamour.Infrastructure.Repositories.InventoryRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouse.Repositories.IProductWarehouseStockRepository,
                           Lamour.Infrastructure.Repositories.ProductWarehouseStockRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Warehouse.UseCases.IGetInventorySummaryUseCase,
                           Lamour.Application.Features.Warehouse.UseCases.GetInventorySummaryUseCase>();

// ── Accounting DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Accounting.Repositories.ICashLedgerRepository,
                           Lamour.Infrastructure.Repositories.CashLedgerRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetCashLedgerUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetCashLedgerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.Repositories.IReceiptRepository,
                           Lamour.Infrastructure.Repositories.ReceiptRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetReceiptsUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetReceiptsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetReceiptByIdUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetReceiptByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.ICreateReceiptUseCase,
                           Lamour.Application.Features.Accounting.UseCases.CreateReceiptUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IUpdateReceiptUseCase,
                           Lamour.Application.Features.Accounting.UseCases.UpdateReceiptUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IDeleteReceiptUseCase,
                           Lamour.Application.Features.Accounting.UseCases.DeleteReceiptUseCase>();

// Payment UseCases
builder.Services.AddScoped<Lamour.Application.Features.Accounting.Repositories.IPaymentRepository,
                           Lamour.Infrastructure.Repositories.PaymentRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetPaymentsUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetPaymentsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetPaymentByIdUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetPaymentByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.ICreatePaymentUseCase,
                           Lamour.Application.Features.Accounting.UseCases.CreatePaymentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IUpdatePaymentUseCase,
                           Lamour.Application.Features.Accounting.UseCases.UpdatePaymentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IDeletePaymentUseCase,
                           Lamour.Application.Features.Accounting.UseCases.DeletePaymentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IDuplicatePaymentUseCase,
                           Lamour.Application.Features.Accounting.UseCases.DuplicatePaymentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IConfirmPaymentUseCase,
                           Lamour.Application.Features.Accounting.UseCases.ConfirmPaymentUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.ISetPaymentTreoUseCase,
                           Lamour.Application.Features.Accounting.UseCases.SetPaymentTreoUseCase>();

// ── WarehouseReceipts DI ──────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.WarehouseReceipts.Repositories.IWarehouseReceiptRepository,
                           Lamour.Infrastructure.Repositories.WarehouseReceiptRepository>();
builder.Services.AddScoped<Lamour.Application.Features.WarehouseReceipts.UseCases.ICreateWarehouseReceiptUseCase,
                           Lamour.Application.Features.WarehouseReceipts.UseCases.CreateWarehouseReceiptUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.WarehouseReceipts.UseCases.IGetWarehouseReceiptsUseCase,
                           Lamour.Application.Features.WarehouseReceipts.UseCases.GetWarehouseReceiptsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.WarehouseReceipts.UseCases.IGetWarehouseReceiptByIdUseCase,
                           Lamour.Application.Features.WarehouseReceipts.UseCases.GetWarehouseReceiptByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.WarehouseReceipts.UseCases.IConfirmWarehouseReceiptUseCase,
                           Lamour.Application.Features.WarehouseReceipts.UseCases.ConfirmWarehouseReceiptUseCase>();

// ── Unit of Work ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Abstractions.IUnitOfWork,
                           Lamour.Infrastructure.Persistence.UnitOfWork>();

// ── Sales DI ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Sales.Repositories.ISalesOrderRepository,
                           Lamour.Infrastructure.Repositories.SalesOrderRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IGetSalesOrdersUseCase,
                           Lamour.Application.Features.Sales.UseCases.GetSalesOrdersUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IGetSalesOrderByIdUseCase,
                           Lamour.Application.Features.Sales.UseCases.GetSalesOrderByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.ICreateSalesOrderUseCase,
                           Lamour.Application.Features.Sales.UseCases.CreateSalesOrderUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IUpdateSalesOrderUseCase,
                           Lamour.Application.Features.Sales.UseCases.UpdateSalesOrderUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IDeleteSalesOrderUseCase,
                           Lamour.Application.Features.Sales.UseCases.DeleteSalesOrderUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IGetNextSalesOrderCodeUseCase,
                           Lamour.Application.Features.Sales.UseCases.GetNextSalesOrderCodeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IHoldSalesOrderUseCase,
                           Lamour.Application.Features.Sales.UseCases.HoldSalesOrderUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IGetSalesOrderReportUseCase,
                           Lamour.Application.Features.Sales.UseCases.GetSalesOrderReportUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Sales.UseCases.IGetSalesOrderSummaryReportUseCase,
                           Lamour.Application.Features.Sales.UseCases.GetSalesOrderSummaryReportUseCase>();

// ── SalesReturn DI ────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.Repositories.ISalesReturnRepository,
                           Lamour.Infrastructure.Repositories.SalesReturnRepository>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.IGetSalesReturnsUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.GetSalesReturnsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.IGetSalesReturnByIdUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.GetSalesReturnByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.IGetNextSalesReturnCodeUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.GetNextSalesReturnCodeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.ICreateSalesReturnUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.CreateSalesReturnUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.IUpdateSalesReturnUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.UpdateSalesReturnUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.SalesReturn.UseCases.IDeleteSalesReturnUseCase,
                           Lamour.Application.Features.SalesReturn.UseCases.DeleteSalesReturnUseCase>();

// ── Deposits DI ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Deposits.Repositories.IDepositRepository,
                           Lamour.Infrastructure.Repositories.DepositRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.Repositories.IDepositDeductionRepository,
                           Lamour.Infrastructure.Repositories.DepositDeductionRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetDepositsUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetDepositsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetDepositByIdUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetDepositByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetNextDepositCodeUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetNextDepositCodeUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetDepositsByCustomerUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetDepositsByCustomerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.ICreateDepositUseCase,
                           Lamour.Application.Features.Deposits.UseCases.CreateDepositUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IUpdateDepositUseCase,
                           Lamour.Application.Features.Deposits.UseCases.UpdateDepositUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IDeleteDepositUseCase,
                           Lamour.Application.Features.Deposits.UseCases.DeleteDepositUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetDepositDeductionsUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetDepositDeductionsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IGetDepositDeductionByIdUseCase,
                           Lamour.Application.Features.Deposits.UseCases.GetDepositDeductionByIdUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.ICreateDepositDeductionUseCase,
                           Lamour.Application.Features.Deposits.UseCases.CreateDepositDeductionUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Deposits.UseCases.IDeleteDepositDeductionUseCase,
                           Lamour.Application.Features.Deposits.UseCases.DeleteDepositDeductionUseCase>();

// ── Backups DI ────────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Backups.Repositories.IBackupRepository,
                           Lamour.Infrastructure.Repositories.BackupRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.IGetBackupsUseCase,
                           Lamour.Application.Features.Backups.UseCases.GetBackupsUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.ICreateBackupUseCase,
                           Lamour.Application.Features.Backups.UseCases.CreateBackupUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.IDeleteBackupUseCase,
                           Lamour.Application.Features.Backups.UseCases.DeleteBackupUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.IRestoreBackupUseCase,
                           Lamour.Application.Features.Backups.UseCases.RestoreBackupUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.Repositories.IBackupScheduleRepository,
                           Lamour.Infrastructure.Repositories.BackupScheduleRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.IGetBackupScheduleUseCase,
                           Lamour.Application.Features.Backups.UseCases.GetBackupScheduleUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Backups.UseCases.IUpdateBackupScheduleUseCase,
                           Lamour.Application.Features.Backups.UseCases.UpdateBackupScheduleUseCase>();
builder.Services.AddHostedService<Lamour.Api.Realtime.BackupSchedulerHostedService>();

// ── Auth DI ───────────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Auth.UseCases.ILoginUseCase,
                           Lamour.Application.Features.Auth.UseCases.LoginUseCase>();

// ── Realtime DI ──────────────────────────────────────────────────────────────
builder.Services.AddSingleton<Lamour.Application.Abstractions.INotificationBroadcaster,
                              SignalRNotificationBroadcaster>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<DataSyncHub>("/hubs/data-sync");

// Auto-migrate on startup (Docker / production)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
