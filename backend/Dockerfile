FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files first for layer caching
COPY Directory.Packages.props ./
COPY src/Bootstrapper/Bootstrapper.csproj src/Bootstrapper/

# Shared projects
COPY src/Shared/Shared.Domain/Shared.Domain.csproj src/Shared/Shared.Domain/
COPY src/Shared/Shared.Contracts/Shared.Contracts.csproj src/Shared/Shared.Contracts/
COPY src/Shared/Shared.Application/Shared.Application.csproj src/Shared/Shared.Application/
COPY src/Shared/Shared.Infrastructure/Shared.Infrastructure.csproj src/Shared/Shared.Infrastructure/
COPY src/Shared/Shared.Presentation/Shared.Presentation.csproj src/Shared/Shared.Presentation/

# Module projects (3 layers each)
COPY src/Modules/Auth/Auth.Domain/Auth.Domain.csproj src/Modules/Auth/Auth.Domain/
COPY src/Modules/Auth/Auth.Contracts/Auth.Contracts.csproj src/Modules/Auth/Auth.Contracts/
COPY src/Modules/Auth/Auth.Application/Auth.Application.csproj src/Modules/Auth/Auth.Application/
COPY src/Modules/Auth/Auth.Infrastructure/Auth.Infrastructure.csproj src/Modules/Auth/Auth.Infrastructure/
COPY src/Modules/Auth/Auth.Presentation/Auth.Presentation.csproj src/Modules/Auth/Auth.Presentation/

COPY src/Modules/Audit/Audit.Domain/Audit.Domain.csproj src/Modules/Audit/Audit.Domain/
COPY src/Modules/Audit/Audit.Contracts/Audit.Contracts.csproj src/Modules/Audit/Audit.Contracts/
COPY src/Modules/Audit/Audit.Application/Audit.Application.csproj src/Modules/Audit/Audit.Application/
COPY src/Modules/Audit/Audit.Infrastructure/Audit.Infrastructure.csproj src/Modules/Audit/Audit.Infrastructure/
COPY src/Modules/Audit/Audit.Presentation/Audit.Presentation.csproj src/Modules/Audit/Audit.Presentation/

COPY src/Modules/Patient/Patient.Domain/Patient.Domain.csproj src/Modules/Patient/Patient.Domain/
COPY src/Modules/Patient/Patient.Contracts/Patient.Contracts.csproj src/Modules/Patient/Patient.Contracts/
COPY src/Modules/Patient/Patient.Application/Patient.Application.csproj src/Modules/Patient/Patient.Application/
COPY src/Modules/Patient/Patient.Infrastructure/Patient.Infrastructure.csproj src/Modules/Patient/Patient.Infrastructure/
COPY src/Modules/Patient/Patient.Presentation/Patient.Presentation.csproj src/Modules/Patient/Patient.Presentation/

COPY src/Modules/Clinical/Clinical.Domain/Clinical.Domain.csproj src/Modules/Clinical/Clinical.Domain/
COPY src/Modules/Clinical/Clinical.Contracts/Clinical.Contracts.csproj src/Modules/Clinical/Clinical.Contracts/
COPY src/Modules/Clinical/Clinical.Application/Clinical.Application.csproj src/Modules/Clinical/Clinical.Application/
COPY src/Modules/Clinical/Clinical.Infrastructure/Clinical.Infrastructure.csproj src/Modules/Clinical/Clinical.Infrastructure/
COPY src/Modules/Clinical/Clinical.Presentation/Clinical.Presentation.csproj src/Modules/Clinical/Clinical.Presentation/

COPY src/Modules/Scheduling/Scheduling.Domain/Scheduling.Domain.csproj src/Modules/Scheduling/Scheduling.Domain/
COPY src/Modules/Scheduling/Scheduling.Contracts/Scheduling.Contracts.csproj src/Modules/Scheduling/Scheduling.Contracts/
COPY src/Modules/Scheduling/Scheduling.Application/Scheduling.Application.csproj src/Modules/Scheduling/Scheduling.Application/
COPY src/Modules/Scheduling/Scheduling.Infrastructure/Scheduling.Infrastructure.csproj src/Modules/Scheduling/Scheduling.Infrastructure/
COPY src/Modules/Scheduling/Scheduling.Presentation/Scheduling.Presentation.csproj src/Modules/Scheduling/Scheduling.Presentation/

COPY src/Modules/Pharmacy/Pharmacy.Domain/Pharmacy.Domain.csproj src/Modules/Pharmacy/Pharmacy.Domain/
COPY src/Modules/Pharmacy/Pharmacy.Contracts/Pharmacy.Contracts.csproj src/Modules/Pharmacy/Pharmacy.Contracts/
COPY src/Modules/Pharmacy/Pharmacy.Application/Pharmacy.Application.csproj src/Modules/Pharmacy/Pharmacy.Application/
COPY src/Modules/Pharmacy/Pharmacy.Infrastructure/Pharmacy.Infrastructure.csproj src/Modules/Pharmacy/Pharmacy.Infrastructure/
COPY src/Modules/Pharmacy/Pharmacy.Presentation/Pharmacy.Presentation.csproj src/Modules/Pharmacy/Pharmacy.Presentation/

COPY src/Modules/Optical/Optical.Domain/Optical.Domain.csproj src/Modules/Optical/Optical.Domain/
COPY src/Modules/Optical/Optical.Contracts/Optical.Contracts.csproj src/Modules/Optical/Optical.Contracts/
COPY src/Modules/Optical/Optical.Application/Optical.Application.csproj src/Modules/Optical/Optical.Application/
COPY src/Modules/Optical/Optical.Infrastructure/Optical.Infrastructure.csproj src/Modules/Optical/Optical.Infrastructure/
COPY src/Modules/Optical/Optical.Presentation/Optical.Presentation.csproj src/Modules/Optical/Optical.Presentation/

COPY src/Modules/Billing/Billing.Domain/Billing.Domain.csproj src/Modules/Billing/Billing.Domain/
COPY src/Modules/Billing/Billing.Contracts/Billing.Contracts.csproj src/Modules/Billing/Billing.Contracts/
COPY src/Modules/Billing/Billing.Application/Billing.Application.csproj src/Modules/Billing/Billing.Application/
COPY src/Modules/Billing/Billing.Infrastructure/Billing.Infrastructure.csproj src/Modules/Billing/Billing.Infrastructure/
COPY src/Modules/Billing/Billing.Presentation/Billing.Presentation.csproj src/Modules/Billing/Billing.Presentation/

COPY src/Modules/Treatment/Treatment.Domain/Treatment.Domain.csproj src/Modules/Treatment/Treatment.Domain/
COPY src/Modules/Treatment/Treatment.Contracts/Treatment.Contracts.csproj src/Modules/Treatment/Treatment.Contracts/
COPY src/Modules/Treatment/Treatment.Application/Treatment.Application.csproj src/Modules/Treatment/Treatment.Application/
COPY src/Modules/Treatment/Treatment.Infrastructure/Treatment.Infrastructure.csproj src/Modules/Treatment/Treatment.Infrastructure/
COPY src/Modules/Treatment/Treatment.Presentation/Treatment.Presentation.csproj src/Modules/Treatment/Treatment.Presentation/

RUN dotnet restore src/Bootstrapper/Bootstrapper.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish src/Bootstrapper/Bootstrapper.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5255
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5255

ENTRYPOINT ["dotnet", "Bootstrapper.dll"]
