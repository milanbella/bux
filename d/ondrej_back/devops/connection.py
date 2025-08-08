from fabric import Connection
import ondrej_back.devops.config

def connectionTestServer(ssh_key_file_name=None):
    key_file = ssh_key_file_name or gao.devops.config.SSH_KEY_FILE_NAME
    conn = Connection(
        ondrej_back.devops.config.TEST_SERVER_NAME,
        user="root",
        connect_kwargs={
            "key_filename": key_file
        }
    )
    return conn
