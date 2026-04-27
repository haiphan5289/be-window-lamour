using Lamour.Api.Middleware;
using Lamour.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

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
builder.Services.AddScoped<Lamour.Application.Features.Warehouse.UseCases.IGetInventorySummaryUseCase,
                           Lamour.Application.Features.Warehouse.UseCases.GetInventorySummaryUseCase>();

// ── Accounting DI ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Accounting.Repositories.ICashLedgerRepository,
                           Lamour.Infrastructure.Repositories.CashLedgerRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetCashLedgerUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetCashLedgerUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.Repositories.IPaymentReceiptRepository,
                           Lamour.Infrastructure.Repositories.PaymentReceiptRepository>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.ICreatePaymentReceiptUseCase,
                           Lamour.Application.Features.Accounting.UseCases.CreatePaymentReceiptUseCase>();
builder.Services.AddScoped<Lamour.Application.Features.Accounting.UseCases.IGetPaymentReceiptsUseCase,
                           Lamour.Application.Features.Accounting.UseCases.GetPaymentReceiptsUseCase>();

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

// ── Auth DI ───────────────────────────────────────────────────────────────────
builder.Services.AddScoped<Lamour.Application.Features.Auth.UseCases.ILoginUseCase,
                           Lamour.Application.Features.Auth.UseCases.LoginUseCase>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
