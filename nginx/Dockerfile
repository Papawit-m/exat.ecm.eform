# ใช้ official NGINX image
FROM nginx:alpine

# คัดลอกไฟล์การตั้งค่า NGINX
COPY nginx.conf /etc/nginx/nginx.conf

# เปิด port 5001 เพื่อให้ NGINX สามารถรับ traffic ได้
EXPOSE 5001

# รัน Nginx ใน foreground
CMD ["nginx", "-g", "daemon off;"] 