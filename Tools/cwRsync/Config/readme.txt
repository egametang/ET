// �������ļ������߱�����root
sudo chown root.root /etc/rsyncd.secrets
sudo chown root.root /etc/rsyncd.conf

// /etc/rsyncd.secrets������600Ȩ�ޣ����򱨴�
// @ERROR: auth failed on module Upload
// rsync error: error starting client-server protocol (code 5) at main.c(1635) [sender=3.1.1]
sudo chmod 600 /etc/rsyncd.secrets

// ���������rsync����
sudo rsync --daemon