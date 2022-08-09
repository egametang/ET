#!/bin/bash

root=`pwd`

link_root="$root/Unity/Assets/Scripts/Codes/Link/"
server_root="$root/Unity/Assets/Scripts/Codes/Link/Generate/"

[ ! -d $server_root ] && mkdir -p $server_root

ln -s $root/Codes/Hotfix $link_root
ln -s $root/Codes/Model $link_root
ln -s $root/Codes/ModelView $link_root
ln -s $root/Codes/HotfixView $link_root
ln -s $root/Codes/Generate/Server $server_root