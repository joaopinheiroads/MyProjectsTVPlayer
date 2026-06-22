Para iniciar aplicação, deve instalar node na raíz do projeto:

 npm install
 npm run build


Para rodar back-end:

 node scraper.js

Node deve startar automaticamente após queda do servidor.
Em casa de falha de start automático, deve-se utilizar o comando: pm2 start scraper.js --name impostometro na raíz do projeto. Para rodar o node sem travar o terminal.
E realizar a tentativa de fazer com que o pm2 obrigue o Windows a manter o serviço rodando mesmo após reiniciar. Para isso reutilizar os comandos: 

 pm2 save
 pm2 status

