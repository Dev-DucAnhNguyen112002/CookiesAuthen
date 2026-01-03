# =========================
# GIAI ĐOẠN 1: BUILD IMAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copy các file .csproj (Giữ đúng cấu trúc thư mục)
# Nếu dự án bạn có file Directory.Build.props hoặc global.json thì PHẢI copy vào
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
# Việc này giúp tận dụng Cache của Docker, build lại sẽ siêu nhanh
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Web/Web.csproj", "src/Web/"]

# 2. Restore các thư viện (Chỉ cần restore project chính, nó sẽ tự kéo các project kia)
RUN dotnet restore "src/Web/Web.csproj"

# 3. Copy toàn bộ source code còn lại
COPY . .

# 4. Chuyển vào thư mục Web và Build ra file thực thi
WORKDIR "/src/src/Web"
RUN dotnet publish "Web.csproj" -c Release -o /app/publish

# =========================
# GIAI ĐOẠN 2: RUNTIME IMAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy kết quả đã build ở giai đoạn 1 sang giai đoạn này
COPY --from=build /app/publish .

# Mở cổng 8080 (Cổng mặc định của .NET 9 trong Docker)
EXPOSE 8080

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "CookiesAuthen.Web.dll"]