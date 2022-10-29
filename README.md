# example-rabbitmq-project-with-dotnet

## Postgresql
### A-Migration for CreateExcelApp (CodeFirst)
```
dotnet ef migrations add first_migration -c AppDbContext
```
### B-Import Adventureworks Product Data for CreateFileWorkerService => DBFirst:

Run queries in Postgresql Query Tool in CreateFileWorkerService:

- 1-product_schema.sql

- 2-product_data.sql

And run below command in terminal in CreateFileWorkerService folder:

```
dotnet ef dbcontext scaffold "Host=localhost;Database=Adventureworks;Username=postgres;Password=postgres" Npgsql.EntityFrameworkCore.PostgreSQL -o Models
```
