from invoke import task
import ondrej_back.devops.connection
import ondrej_back.devops.deploy
import os


@task
def deploy_ondrej_back_to_test(c, branch = "main", ssh_key_file_name = ondrej_back.devops.config.SSH_KEY_FILE_NAME):
    sconn = ondrej_back.devops.connection.connectionTestServer(ssh_key_file_name)
    ondrej_back.devops.deploy.deploy_ondrej_back_to_test(sconn, branch)
