pipeline {
    agent any

    // 1. Định nghĩa các biến môi trường
    environment {
        // Tên ảnh và container
        IMAGE_NAME = 'cookies-authen-app'
        CONTAINER_NAME = 'my-web-app'
        
        // Cấu hình Telegram (Lấy từ Credentials như bài trước mình chỉ)
        BOT_TOKEN = credentials('TELEGRAM_TOKEN')
        CHAT_ID = credentials('TELEGRAM_CHAT_ID')
    }

    options {
        // Giới hạn thời gian chạy 10 phút thôi cho đỡ treo máy
        timeout(time: 10, unit: 'MINUTES')
        // Không cho chạy 2 job cùng lúc
        disableConcurrentBuilds()
    }

    stages {
        stage('🛠️ Check Environment') {
            steps {
                script {
                    echo "🚀 Building Branch: ${env.BRANCH_NAME}"
                    // Kiểm tra Docker có sống không
                    sh 'docker --version || { echo "❌ Docker chưa cài!"; exit 1; }'
                }
            }
        }

        // Bước Clone Code: Jenkins tự làm nếu bạn cấu hình Git trong Job rồi.
        // Nếu dùng Jenkinsfile trong Git thì nó tự checkout luôn, không cần stage Clone.

        stage('🐳 Build Docker Image') {
            steps {
                echo 'Building Docker image...'
                // Build ảnh, gắn tag là số lần build (BUILD_NUMBER)
                sh "docker build -t ${IMAGE_NAME}:${env.BUILD_NUMBER} -t ${IMAGE_NAME}:latest ."
            }
        }

        stage('🚀 Deploy to Local') {
            steps {
                echo 'Deploying to Localhost...'
                script {
                    // Stop & Remove container cũ
                    sh "docker stop ${CONTAINER_NAME} || true"
                    sh "docker rm ${CONTAINER_NAME} || true"

                    // Chạy container mới (Dùng lệnh IP LAN của bạn)
                    sh """
                        docker run -d -p 5000:8080 \
                        --name ${CONTAINER_NAME} \
                        -e ASPNETCORE_ENVIRONMENT=Docker \
                        ${IMAGE_NAME}:latest
                    """
                }
            }
        }
    }

    post {
        always {
            echo '🧹 Cleaning up...'
            // Xóa ảnh rác để đỡ tốn ổ cứng laptop
            sh "docker image prune -f"
        }

        success {
            script {
                def message = "✅ <b>DEPLOY SUCCESS!</b>%0A" +
                              "📦 Job: ${env.JOB_NAME}%0A" +
                              "🔢 Build: #${env.BUILD_NUMBER}%0A" +
                              "🌿 Branch: ${env.BRANCH_NAME}%0A" +
                              "------------------------------%0A" +
                              "Server đã lên sóng!"
                sendTelegram(message)
            }
        }

        failure {
            script {
                // Lấy link log để bấm vào xem cho nhanh
                def logLink = "${env.JENKINS_URL}job/${env.JOB_NAME}/${env.BUILD_NUMBER}/console"
                def message = "❌ <b>DEPLOY FAILED!</b>%0A" +
                              "📦 Job: ${env.JOB_NAME}%0A" +
                              "🔢 Build: #${env.BUILD_NUMBER}%0A" +
                              "🔗 <a href='${logLink}'>Xem Log chi tiết</a>"
                sendTelegram(message)
            }
        }
    }
}

// Hàm gửi Telegram (Mình viết gọn lại cho dễ nhìn)
def sendTelegram(msg) {
    if (env.BOT_TOKEN && env.CHAT_ID) {
        sh """
            curl -s -X POST https://api.telegram.org/bot${env.BOT_TOKEN}/sendMessage \
            -d chat_id=${env.CHAT_ID} \
            -d parse_mode=HTML \
            -d text=\"${msg}\"
        """
    } else {
        echo "⚠️ Không tìm thấy Token Telegram, bỏ qua gửi tin nhắn."
    }
}