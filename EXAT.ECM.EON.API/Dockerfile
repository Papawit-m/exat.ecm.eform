#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# ติดตั้ง tzdata เพื่อรองรับ Timezone
USER root
RUN apt-get update && apt-get install -y tzdata && \
    ln -sf /usr/share/zoneinfo/Asia/Bangkok /etc/localtime && \
    echo "Asia/Bangkok" > /etc/timezone

# ตั้งค่า Timezone เป็น Asia/Bangkok
ENV TZ=Asia/Bangkok


USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EXAT.ECM.EON.API/EXAT.ECM.EON.API.csproj", "EXAT.ECM.EON.API/"]
RUN dotnet restore "./EXAT.ECM.EON.API/EXAT.ECM.EON.API.csproj"
COPY . .
WORKDIR "/src/EXAT.ECM.EON.API"
RUN dotnet build "./EXAT.ECM.EON.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EXAT.ECM.EON.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EXAT.ECM.EON.API.dll"]

# FONTS
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
COPY ["EXAT.ECM.EON.API/DocumentTemplate/EON/EONSummaryReportTemplate.docx", "/app/DocumentTemplate/EON/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/EON/EONRequestFormTemplate.docx", "/app/DocumentTemplate/EON/"]

# ASPOSE
COPY ["EXAT.ECM.EON.API/LicenseAspose/AsposeEON.lic", "/app/bin/"]