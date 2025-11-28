#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# ติดตั้ง tzdata เพื่อรองรับ Timezone
USER root
# Timezone + แพ็กเกจระบบที่ Skia/QuestPDF ต้องใช้ + ICU สำหรับรูปแบบภาษา
RUN apt-get update && apt-get install -y \
    tzdata \
    libfontconfig1 \
    libfreetype6 \
    libharfbuzz0b \
    libpng16-16 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libstdc++6 \
 && rm -rf /var/lib/apt/lists/*

# ตั้งค่า Timezone เป็น Asia/Bangkok
RUN ln -sf /usr/share/zoneinfo/Asia/Bangkok /etc/localtime && \
    echo "Asia/Bangkok" > /etc/timezone
ENV TZ=Asia/Bangkok
# เปิดใช้ globalization เต็มรูปแบบ (จำเป็นต่อฟอนต์/ภาษา)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0


USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EXAT.ECM.PPA.API/EXAT.ECM.PPA.API.csproj", "EXAT.ECM.PPA.API/"]
RUN dotnet restore "./EXAT.ECM.PPA.API/EXAT.ECM.PPA.API.csproj"
COPY . .
WORKDIR "/src/EXAT.ECM.PPA.API"
RUN dotnet build "./EXAT.ECM.PPA.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EXAT.ECM.PPA.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EXAT.ECM.PPA.API.dll"]

# FONTS
COPY ["EXAT.ECM.PPA.API/DocumentTemplate/Fonts/THSarabun Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.PPA.API/DocumentTemplate/Fonts/THSarabun BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.PPA.API/DocumentTemplate/Fonts/THSarabun Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.PPA.API/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
COPY ["EXAT.ECM.PPA.API/DocumentTemplate/PPA/PPASummaryReportTemplate.docx", "/app/DocumentTemplate/PPA/"]

# ASPOSE
COPY ["EXAT.ECM.PPA.API/LicenseAspose/AsposePPA.Total.NET.lic", "/app/bin/"]
