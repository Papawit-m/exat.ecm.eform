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
COPY ["EXAT.ECM.FED.API/EXAT.ECM.FED.API.csproj", "EXAT.ECM.FED.API/"]
RUN dotnet restore "./EXAT.ECM.FED.API/EXAT.ECM.FED.API.csproj"
COPY . .
WORKDIR "/src/EXAT.ECM.FED.API"
RUN dotnet build "./EXAT.ECM.FED.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EXAT.ECM.FED.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EXAT.ECM.FED.API.dll"]


# FONTS
COPY ["EXAT.ECM.FED.API/DocumentTemplate/Fonts/THSarabun Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/Fonts/THSarabun BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/Fonts/THSarabun Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleRequestTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDDailyVehiUsageTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDMonthlyVehicleUsageTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDDriverUsageVehicleTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDMaChineUseTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDFuelExpenseReqTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDPoliceFuelExceedTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDIncomptFuelTaxInvTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDFuelFleetCardTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/FEDFuelFleetCardBankTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleRepairRequestTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/DailyVehicleInspectionTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleHandoverTemplate.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleInspectionDeliveryTemplate1.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleInspectionDeliveryTemplate2.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleInspectionDeliveryTemplate3.docx", "/app/DocumentTemplate/FED/"]
COPY ["EXAT.ECM.FED.API/DocumentTemplate/FED/VehicleInspectionDeliveryTemplate4.docx", "/app/DocumentTemplate/FED/"]



# ASPOSE
COPY ["EXAT.ECM.FED.API/LicenseAspose/AsposeFED.Total.NET.lic", "/app/bin/"]
