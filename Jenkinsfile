pipeline {
    agent any

    environment {
        BOT_TOKEN = credentials('TELEGRAM_TOKEN')
        CHAT_ID   = credentials('TELEGRAM_CHAT_ID')
        // Biến này để Docker Compose đọc
        ASPNETCORE_ENVIRONMENT = "Testing" 
    }

    stages {
        stage('♻️ Checkout Code') {
            steps {
                checkout scm
                echo 'Đã kéo code mới nhất về!'
            }
        }

        stage('🚀 Build & Deploy (Docker Compose)') {
            steps {
                script {
                    echo 'Đang Build và Deploy bằng Docker Compose...'
                    // Lệnh DUY NHẤT bạn cần. 
                    // Nó tự Build -> Tự Stop cũ -> Tự Run mới -> Tự Map port
                    sh "docker compose up -d --build"
                }
            }
        }
        
        // ❌ ĐÃ XÓA STAGE "Deploy to Container" (docker run) Ở ĐÂY VÌ NÓ BỊ THỪA
    }

    post {
        always {
            sh 'docker image prune -f' 
        }
        success {
            script {
                sendTelegram("✅ <b>DEPLOY SUCCESS!</b>%0AApp đã chạy ngon lành trên cổng 5000!")
            }
        }
        failure {
            script {
                sendTelegram("❌ <b>DEPLOY FAILED!</b>%0AKiểm tra lại ngay!")
            }
        }
    }
}

def sendTelegram(message) {
    if (env.BOT_TOKEN && env.CHAT_ID) {
        sh "curl -s -X POST https://api.telegram.org/bot${BOT_TOKEN}/sendMessage -d chat_id=${CHAT_ID} -d parse_mode=HTML -d text=\"${message}\""
    }
}