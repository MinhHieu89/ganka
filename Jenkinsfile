pipeline {
    agent any

    triggers {
        pollSCM('H/10 * * * *')
    }

    environment {
        HARBOR_REGISTRY = 'harbor.labs.local'
        HARBOR_PROJECT  = 'ganka'
        IMAGE_TAG       = "${env.BUILD_NUMBER}-${env.GIT_COMMIT?.take(7) ?: 'unknown'}"
        DEPLOY_HOST     = credentials('ganka-deploy-host')
        HARBOR_CREDS    = credentials('harbor-credentials')
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Test Backend') {
            steps {
                dir('backend') {
                    sh 'dotnet restore src/Bootstrapper/Bootstrapper.csproj'
                    sh 'dotnet build src/Bootstrapper/Bootstrapper.csproj -c Release --no-restore'
                }
            }
        }

        stage('Build Images') {
            parallel {
                stage('Backend Image') {
                    steps {
                        dir('backend') {
                            sh """
                                docker build \
                                    -t ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/backend:${IMAGE_TAG} \
                                    -t ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/backend:latest \
                                    .
                            """
                        }
                    }
                }
                stage('Frontend Image') {
                    steps {
                        dir('frontend') {
                            sh """
                                docker build \
                                    --build-arg VITE_API_URL=${VITE_API_URL} \
                                    -t ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/frontend:${IMAGE_TAG} \
                                    -t ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/frontend:latest \
                                    .
                            """
                        }
                    }
                }
            }
        }

        stage('Push to Harbor') {
            steps {
                sh "echo ${HARBOR_CREDS_PSW} | docker login ${HARBOR_REGISTRY} -u ${HARBOR_CREDS_USR} --password-stdin"
                sh """
                    docker push ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/backend:${IMAGE_TAG}
                    docker push ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/backend:latest
                    docker push ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/frontend:${IMAGE_TAG}
                    docker push ${HARBOR_REGISTRY}/${HARBOR_PROJECT}/frontend:latest
                """
            }
        }

        stage('Deploy') {
            steps {
                sshagent(credentials: ['ganka-deploy-ssh']) {
                    sh """
                        ssh -o StrictHostKeyChecking=no ${DEPLOY_HOST} << 'ENDSSH'
                            cd /opt/ganka
                            echo ${HARBOR_CREDS_PSW} | docker login ${HARBOR_REGISTRY} -u ${HARBOR_CREDS_USR} --password-stdin
                            export IMAGE_TAG=${IMAGE_TAG}
                            docker compose pull
                            docker compose up -d --remove-orphans
                            docker image prune -f
ENDSSH
                    """
                }
            }
        }
    }

    post {
        always {
            sh "docker logout ${HARBOR_REGISTRY} || true"
            cleanWs()
        }
        failure {
            echo "Build failed for ${env.JOB_NAME} #${env.BUILD_NUMBER}"
        }
        success {
            echo "Deployed ${IMAGE_TAG} successfully"
        }
    }
}
