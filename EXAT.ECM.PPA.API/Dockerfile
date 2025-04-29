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

# FONTS
COPY ["EXAT.ECM.Business/DocumentTemplate/Fonts/THSarabun Bold.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.Business/DocumentTemplate/Fonts/THSarabun BoldItalic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.Business/DocumentTemplate/Fonts/THSarabun Italic.ttf", "/app/DocumentTemplate/Fonts/"]
COPY ["EXAT.ECM.Business/DocumentTemplate/Fonts/THSarabun.ttf", "/app/DocumentTemplate/Fonts/"]

# TEMPLATE
# TEMPLATE
#COPY ["EXAT.ECM.Business/DocumentTemplate/LCI/LCISummaryReportTemplate.docx", "/app/DocumentTemplate/LCI/"]
#COPY ["EXAT.ECM.Business/DocumentTemplate/PRS/PRSSummaryReportTemplate.docx", "/app/DocumentTemplate/PRS/"]
#COPY ["EXAT.ECM.Business/DocumentTemplate/EON/EONSummaryReportTemplate.docx", "/app/DocumentTemplate/EON/"]
COPY ["EXAT.ECM.Business/DocumentTemplate/PPA/PPASummaryReportTemplate.docx", "/app/DocumentTemplate/PPA/"]

##COPY ["EXAT.ECM.Business/DocumentTemplate/EER/EONSummaryReportTemplate.docx", "/app/DocumentTemplate/EER/"]

# ASPOSE
COPY ["EXAT.ECM.Business/Aspose.lic", "/app/bin/"]

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