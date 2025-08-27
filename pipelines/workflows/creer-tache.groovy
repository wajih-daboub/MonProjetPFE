pipeline {
  agent any
  parameters {
    string(name: 'title', defaultValue: '', description: 'Titre de la tâche')
  }
  stages {
    stage('Créer Tâche (exemple)') {
      steps {
        echo "Création de la tâche: ${params.title}"
        // Exemple d'appel vers ton système métier (remplace l’URL):
        sh '''
        curl -s -X POST http://platform-internal/api/tasks \
          -H "Content-Type: application/json" \
          -d '{"title":"'"${title}"'","status":"NEW"}' || true
        '''
      }
    }
  }
}
