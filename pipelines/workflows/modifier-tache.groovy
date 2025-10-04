pipeline {
  agent any
  parameters {
    string(name: 'id', defaultValue: '', description: 'Id de la tâche')
    string(name: 'title', defaultValue: '', description: 'Nouveau titre (optionnel)')
    string(name: 'status', defaultValue: '', description: 'Nouveau statut (optionnel)')
  }
  stages {
    stage('Modifier Tâche (exemple)') {
      steps {
        echo "Modification id=${params.id} title=${params.title} status=${params.status}"
        sh '''
        curl -s -X PUT http://platform-internal/api/tasks/'"${id}"' \
          -H "Content-Type: application/json" \
          -d '{"title":"'"${title}"'","status":"'"${status}"'"}' || true
        '''
      }
    }
  }
}
