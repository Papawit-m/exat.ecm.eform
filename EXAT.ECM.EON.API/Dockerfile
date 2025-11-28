# ================= BASE IMAGE ==================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

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

# ================= BUILD IMAGE ==================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["EXAT.ECM.EON.API/EXAT.ECM.EON.API.csproj", "EXAT.ECM.EON.API/"]
RUN dotnet restore "./EXAT.ECM.EON.API/EXAT.ECM.EON.API.csproj"

COPY . .
WORKDIR "/src/EXAT.ECM.EON.API"
RUN dotnet build "./EXAT.ECM.EON.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ================= PUBLISH ==================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EXAT.ECM.EON.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ================= FINAL IMAGE ==================
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# FONTS
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun-Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun-BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun-Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
COPY ["EXAT.ECM.EON.API/DocumentTemplate/EON/EONSummaryReportTemplate.docx", "/app/DocumentTemplate/EON/"]
COPY ["EXAT.ECM.EON.API/DocumentTemplate/EON/EONRequestFormTemplate.docx", "/app/DocumentTemplate/EON/"]

# ASPOSE LICENSE
COPY ["EXAT.ECM.EON.API/LicenseAspose/AsposeEON.Total.NET.lic", "/app/"]

ENTRYPOINT ["dotnet", "EXAT.ECM.EON.API.dll"]
