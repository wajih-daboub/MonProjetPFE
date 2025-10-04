pipeline {
  agent any
  parameters {
    string(name: 'title', defaultValue: '', description: 'Titre de la tâche')
  }
  stages {
    stage('Créer Tâche (exemple)') {
      steps {
        echo "Création de la tâche: ${params.title}"
        // Ici éventuellement tes vraies étapes métier côté Jenkins
      }
    }
  }
}
