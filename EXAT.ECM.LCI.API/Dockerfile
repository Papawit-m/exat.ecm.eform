# =========================
# Base runtime (Debian/glibc)
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

# ต้อง root เพื่อ apt-get
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

# ผูกพอร์ตเดียวตามที่แอพฟังอยู่ เพื่อลด warning
WORKDIR /app
EXPOSE 5005
ENV ASPNETCORE_URLS=http://+:5005

# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# restore ตามโครงสร้างเดิมของโปรเจ็กต์
COPY ["EXAT.ECM.LCI.API/EXAT.ECM.LCI.API.csproj", "EXAT.ECM.LCI.API/"]
RUN dotnet restore "./EXAT.ECM.LCI.API/EXAT.ECM.LCI.API.csproj"

# คัดลอกซอร์สทั้งหมด แล้ว build
COPY . .
WORKDIR "/src/EXAT.ECM.LCI.API"
RUN dotnet build "./EXAT.ECM.LCI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# =========================
# Publish stage
# =========================
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# สำคัญ: publish เป็น linux-x64 เพื่อให้ดึง native .so มาด้วย
# (ต้องมีแพ็กเกจ SkiaSharp.NativeAssets.Linux.NoDependencies ใน .csproj)
RUN dotnet publish "./EXAT.ECM.LCI.API.csproj" -c $BUILD_CONFIGURATION \
    -r linux-x64 --self-contained false -o /app/publish /p:UseAppHost=false

# =========================
# Final image
# =========================
FROM base AS final
WORKDIR /app

# คัดลอกไฟล์ publish ทั้งหมด (รวม native .so ถ้า .csproj ตั้งถูก)
COPY --from=publish /app/publish .

# คัดลอกฟอนต์/เทมเพลต/ไลเซนส์เข้าไปยัง path ที่โค้ดใช้งานจริง
# (ถ้าแอพอ้าง path อื่น ให้แก้ปลายทางให้ตรง)
# FONTS
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/Fonts/THSarabun Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/Fonts/THSarabun BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/Fonts/THSarabun Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/LCI/LCISummaryReportTemplate.docx", "/app/DocumentTemplate/LCI/"]
COPY ["EXAT.ECM.LCI.API/DocumentTemplate/LCI/LCIRequestFormTemplate.docx", "/app/DocumentTemplate/LCI/"]

# ASPOSE LICENSE
COPY ["EXAT.ECM.LCI.API/LicenseAspose/AsposeLCI.Total.NET.lic", "/app/bin/"]

# (ออปชัน) ตรวจว่า libSkiaSharp.so โผล่มาแล้ว ช่วยไล่ปัญหาถ้า publish ไม่พา .so มาด้วย
# RUN ls -la /app | grep -i libSkiaSharp || (echo "libSkiaSharp.so not found!" && exit 1)

ENTRYPOINT ["dotnet", "EXAT.ECM.LCI.API.dll"]
