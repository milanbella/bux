

def deploy_ondrej_back_to_test(c, branch: str):
    #git_origin = "git@212.24.97.13:repos/gaos.git"
    git_origin = "git@github.com:milanbella/ondrej_back.git"
    cmd = f'''
    set -e
    echo "INFO: getting newest source code"
    if [ ! -d "/opt/ondrej_back" ]; then
        echo "INFO: cloning {git_origin} to /opt/ondrej_back"
        git clone {git_origin} /opt/ondrej_back
    fi
    echo "INFO: git pull ondrej_back"
    cd /opt/ondrej_back
    git reset --hard
    git checkout {branch}
    git pull
    '''
    c.run(cmd)

    cmd = f'''
    set -e
    cd /opt/ondrej_back
    export ASPNETCORE_ENVIRONMENT=Test
    export PATH=$PATH:/root/.dotnet/tools
    export AISHOPS_PASSWORD_FILE=/opt/gao_password.txt

    echo "INFO: stopping ondrej_back service"
    systemctl stop ondrej_back

    echo "INFO: building ondrej_back"
    dotnet build

    echo "INFO: publishing ondrej_back"
    dotnet publish --configuration Release
    chown -R ondrej:ondrej /opt/ondrej_back/bin/Release/net9.0/publish

    echo "INFO: droping database"
    mariadb -e 'drop database if exists ondrej; create database ondrej;'

    echo "INFO: initializing mariadb database"
    rm -rf Migrations
    dotnet ef migrations add InitialCreate
    dotnet ef database update

    echo "INFO: starting ondrej_back service"
    systemctl start ondrej_back
    '''
    c.run(cmd)

    pass

