pipeline {
  agent any
  parameters {
    string(name: 'id', defaultValue: '', description: 'Id de la tâche à supprimer')
  }
  stages {
    stage('Supprimer Tâche (exemple)') {
      steps {
        echo "Suppression id=${params.id}"
        sh """
        curl -s -X DELETE http://platform-internal/api/tasks/${params.id} || true
        """
      }
    }
  }
}
