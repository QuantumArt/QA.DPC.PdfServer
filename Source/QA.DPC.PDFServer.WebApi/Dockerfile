FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
RUN apt-get update -qq && apt-get -y install libgdiplus libc6-dev
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["QA.DPC.PDFServer.PdfGenerator/QA.DPC.PDFServer.PdfGenerator.csproj", "QA.DPC.PDFServer.PdfGenerator/"]
COPY ["QA.DPC.PDFServer.Services/QA.DPC.PDFServer.Services.csproj", "QA.DPC.PDFServer.Services/"]
COPY ["QA.DPC.PDFServer.WebApi/QA.DPC.PDFServer.WebApi.csproj", "QA.DPC.PDFServer.WebApi/"]
COPY nuget.config .
RUN dotnet restore "QA.DPC.PDFServer.WebApi/QA.DPC.PDFServer.WebApi.csproj"

COPY . .

WORKDIR "/src/QA.DPC.PDFServer.WebApi"
RUN dotnet build "QA.DPC.PDFServer.WebApi.csproj" -c Release -o /app -f netcoreapp2.2

FROM build AS publish
RUN dotnet publish "QA.DPC.PDFServer.WebApi.csproj" -c Release -o /app -f netcoreapp2.2

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN chmod 755 /app/Rotativa/Linux/wkhtmltopdf
ENTRYPOINT ["dotnet", "QA.DPC.PDFServer.WebApi.dll"]