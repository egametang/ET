(add-to-list 'load-path "~/.emacs.d/el-get/el-get")

(unless (require 'el-get nil t)
  (url-retrieve
   "https://raw.github.com/dimitri/el-get/master/el-get-install.el"
   (lambda (s)
     (goto-char (point-max))
     (eval-print-last-sexp))))

(el-get 'sync)

(setq make-backup-files nil)
(global-set-key [f5] 'revert-buffer-no-confirm)

(defun revert-buffer-no-confirm()
    "刷新buffer不需要yes no"
    (interactive) (revert-buffer t t))