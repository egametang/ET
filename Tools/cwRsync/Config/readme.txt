// 这两个文件所有者必须是root
sudo chown root.root /etc/rsyncd.secrets
sudo chown root.root /etc/rsyncd.conf

// /etc/rsyncd.secrets必须是600权限，否则报错
// @ERROR: auth failed on module Upload
// rsync error: error starting client-server protocol (code 5) at main.c(1635) [sender=3.1.1]
sudo chmod 600 /etc/rsyncd.secrets

// 启动服务端rsync服务
sudo rsync --daemon