pipeline {
    agent any
    
    // Lấy bí mật từ Jenkins ra để dùng
    environment {
        // Tên biến = credentials('ID-ban-da-tao')
        BOT_TOKEN = credentials('TELEGRAM_TOKEN')
        CHAT_ID   = credentials('TELEGRAM_CHAT_ID')
        
        // Cấu hình tên Container và Image
        IMAGE_NAME = "cookies-authen-app"
        CONTAINER_NAME = "my-web-app"
    }

    stages {
        stage('♻️ Checkout Code') {
            steps {
                // Bước này Jenkins tự làm khi kết nối Git, nhưng viết ra cho rõ
                checkout scm
                echo 'Đã kéo code mới nhất về!'
            }
        }

        stage('🔨 Build Docker Image') {
            steps {
                script {
                    // Build image mới
                    sh "docker build -t ${IMAGE_NAME} ."
                }
            }
        }

        stage('🚀 Deploy to Container') {
            steps {
                script {
                    // Dùng lệnh || true để không lỗi nếu container chưa tồn tại
                    sh "docker stop ${CONTAINER_NAME} || true"
                    sh "docker rm ${CONTAINER_NAME} || true"
                    
                    // Chạy Container mới (Dùng config file Docker đã tạo ở bài trước)
                    // Lưu ý: Mình dùng IP 192.168.1.225 như bạn đã test thành công
                    sh """
                        docker run -d -p 5000:8080 \
                        --name ${CONTAINER_NAME} \
                        -e ASPNETCORE_ENVIRONMENT=Docker \
                        ${IMAGE_NAME}
                    """
                }
            }
        }
    }

    // Phần quan trọng: Thông báo sau khi chạy xong
    post {
        always {
            // Dọn dẹp rác image
            sh 'docker image prune -f'
        }
        success {
            script {
                def message = "✅ <b>DEPLOY SUCCESS!</b>%0A" +
                              "📦 Project: ${env.JOB_NAME}%0A" +
                              "🔢 Build: #${env.BUILD_NUMBER}%0A" +
                              "------------------------------%0A" +
                              "🎉 Server đã lên sóng. Check ngay!"
                sendTelegram(message)
            }
        }
        failure {
            script {
                def message = "❌ <b>DEPLOY FAILED!</b>%0A" +
                              "📦 Project: ${env.JOB_NAME}%0A" +
                              "🔢 Build: #${env.BUILD_NUMBER}%0A" +
                              "------------------------------%0A" +
                              "⚠️ Mau vào kiểm tra Log gấp!"
                sendTelegram(message)
            }
        }
    }
}

// Hàm gửi tin nhắn (Viết riêng cho gọn)
def sendTelegram(message) {
    // Dùng lệnh curl có sẵn trong Linux để gọi API Telegram
    sh "curl -s -X POST https://api.telegram.org/bot${BOT_TOKEN}/sendMessage -d chat_id=${CHAT_ID} -d parse_mode=HTML -d text=\"${message}\""
}