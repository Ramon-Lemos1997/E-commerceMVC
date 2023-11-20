# E-commerceMVC
Este é um projeto no qual desenvolvi um E-commerce, a ideia geral é mostrar meus conhecimentos e aprimorar os mesmos, decidi contribuir com a comunidade disponibilizando ele, ele é um E-commerce onde usei referências como mercado livre, amazon, aliExpress etc...,
conta com várias funcionalidades para Admin como gerenciamento de usuários, da loja, de permissões e conta com autenticações para inibir acesso não liberados, somente com a conexão com o banco de dados você conseguirá acessar praticamente todas funcionalidades, mas
se caso ver o projeto funcionando o pagamento e debitando no stock e processando o pedido é necessário configurar um webhook da stripe, deixarei o webhook no github também, e para enviar o Email's também é necessário configurar suas conta no SendGrid, abaixo estará
minha configuração do appsettings, edite com suas configurações e suas chaves;

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Sua string de conexão"

  },
  "SendGridSettings": {
    "User": "Seu user",
    "Key": "Sua chave privada do sendgrind"
  },

  "StripeSettings": {
    "SecretKey": "Sua chave privada da stripe",
    "PublicKey": "Chave pública da stripe"
  },

  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Sua string de conexão",
          "tableName": "Logs",
          "autoCreateSqlTable": true
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/file.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  },

  "AllowedHosts": "*"
}




