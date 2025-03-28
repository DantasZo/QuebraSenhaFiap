# Gerador de Sequência
Bem-vindo ao Gerador de Sequência! 🎉

Este projeto é uma aplicação C# que gera sequências aleatórias e as envia para uma API local para verificar se a sequência está correta. É uma ferramenta útil para testar a robustez de sistemas que dependem de chaves ou senhas.

Funcionalidades
Geração de Sequências Aleatórias: Gera sequências de 4 caracteres, incluindo letras e números.

Envio para API Local: As sequências são enviadas para uma API local configurável, encontrada no repositório: (preciso criar o repo)

Verificação de Sucesso ou Falha: Verifica se a sequência foi aceita ou rejeitada pela API.

Gravação de Tentativas e Chaves: As tentativas e chaves usadas são gravadas periodicamente em arquivos de texto para referência futura.

Execução Paralela: Utiliza múltiplas tarefas paralelas para otimizar a performance.

Requisitos
.NET 8: Requer a versão mais recente do framework .NET.

Visual Studio 2022: Utilize o Visual Studio 2022 para desenvolver e compilar o projeto.

Biblioteca Newtonsoft.Json: Utilizada para manipulação de JSON.

Como Usar
Clone o Repositório: Clone este repositório para o seu ambiente local.

Abra no Visual Studio 2022: Abra o projeto no Visual Studio 2022.

Configure os Caminhos de Arquivos:
var pastaTentativas = @"Caminho/Para/Tentativas";

var pastaChaves = @"Caminho/Para/Chaves";

Configure a URL da API e o Grupo:
var url = "http://sua-api-local.com";

var grupo = "SeuGrupo";

Compile e Execute o Projeto: Compile e execute o projeto no Visual Studio.

Exemplo de Uso
Ao executar o projeto, a aplicação irá gerar sequências aleatórias e enviá-las para a API configurada. As tentativas e chaves usadas serão gravadas em arquivos de texto para referência futura.
Tentativa: A1b2 - Resultado: Falha. Continuando... 
Tentativa: C3d4 - Resultado: Sucesso! Sequência correta encontrada: C3d4~

Agradecimento: Vale lembrar que o projeto foi proposto na aula inaugural da fiap, em minha pós graduação, obrigado Alura e Fiap :) 

