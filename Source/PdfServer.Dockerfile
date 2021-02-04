FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
LABEL stage=intermediate

WORKDIR /app
COPY *.sln nuget.config ./
ADD projectfiles.tar .
RUN dotnet restore

COPY . ./

RUN dotnet publish /app/QA.DPC.PDFServer.WebApi/QA.DPC.PDFServer.WebApi.csproj -c Release -o out -f netcoreapp3.1 --no-restore


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

ARG SERVICE_NAME
ENV SERVICE_NAME=${SERVICE_NAME:-PdfApi}
ARG SERVICE_VERSION
ENV SERVICE_VERSION=${SERVICE_VERSION:-0.0.0.0}

WORKDIR /app
RUN apt-get update -qq && apt-get -y install libgdiplus libc6-dev
COPY --from=build-env /app/QA.DPC.PDFServer.WebApi/out .
RUN chmod 755 /app/Rotativa/Linux/wkhtmltopdf
ENTRYPOINT ["dotnet", "QA.DPC.PDFServer.WebApi.dll"]


