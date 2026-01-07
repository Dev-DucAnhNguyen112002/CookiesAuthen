pipeline {
    agent { label 'docker' }

    environment {
        BOT_TOKEN = credentials('TELEGRAM_TOKEN')
        CHAT_ID   = credentials('TELEGRAM_CHAT_ID')
        // Biến này để Docker Compose đọc
        ASPNETCORE_ENVIRONMENT = "Development" 
        APP_VERSION = "${TAG_NAME ?: "${BRANCH_NAME}.${BUILD_NUMBER}"}"
    }

    stages {
        stage('♻️ Checkout Code') {
            steps {
                checkout scm
                echo 'Đã kéo code mới nhất về!'
            }
        }

        stage('🚀 Build & Deploy (Docker Compose)') {
	    when {
                anyOf {
                    branch 'main'
                    buildingTag()
                }
            }
            steps {
                script {
                    echo "Deploying version: ${APP_VERSION}"
                    // Lệnh DUY NHẤT bạn cần. 
                    // Nó tự Build -> Tự Stop cũ -> Tự Run mới -> Tự Map port
                    //sh "docker compose up -d --build"
                    sh "docker compose build"
                    sh "docker compose up -d"
                }
            }
        }
        
        // ❌ ĐÃ XÓA STAGE "Deploy to Container" (docker run) Ở ĐÂY VÌ NÓ BỊ THỪA
    }

    post {
        always {
	    cleanWs()
            sh 'docker image prune -f --filter "until=48h"' 
        }
        success {
            script {
                sendTelegram("✅ <b>DEPLOY SUCCESS</b>%0AVersion: <b>${APP_VERSION}</b>")
            }
        }
        failure {
            script {
                sendTelegram("❌ <b>DEPLOY FAILED</b>%0AVersion: ${APP_VERSION}")
            }
        }
    }
}

def sendTelegram(message) {
    if (env.BOT_TOKEN && env.CHAT_ID) {
        sh "curl -s -X POST https://api.telegram.org/bot${BOT_TOKEN}/sendMessage -d chat_id=${CHAT_ID} -d parse_mode=HTML -d text=\"${message}\""
    }
}