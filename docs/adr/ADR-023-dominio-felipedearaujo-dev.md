# ADR-023: Domínio felipedearaujo.dev com Roteamento por Path

**Status:** Aceito
**Data:** 2026-06-30
**Requisitos:** RNF06, RNF11
**Relaciona-se com:** complementa o [ADR-015](ADR-015-ec2-arm-graviton.md) (hospedagem) e o [ADR-017](ADR-017-terraform-iac.md) (Terraform); fecha a pendência de HTTPS deixada em aberto na Fase 5 do [roteiro](../../.ia/roteiro_desenvolvimento.md).

---

## Contexto

O sistema só era acessível pelo IP bruto da EC2, sem HTTPS — a Fase 5 do roteiro já registrava essa pendência ("HTTPS/certbot requer domínio"). O autor possui o domínio pessoal `felipedearaujo.dev`, registrado direto no Route 53 na mesma conta AWS, e quer reservar a raiz do domínio para uso futuro (ex: portfólio pessoal), com o Visiosys respondendo num path dedicado.

## Decisão

- **Path em vez de subdomínio:** o sistema responde em `felipedearaujo.dev/visiosys`, não em `visiosys.felipedearaujo.dev`. Um único certificado TLS cobre o domínio inteiro, sem precisar de wildcard, e a raiz do domínio fica livre para outro conteúdo no futuro.
- **`app.UsePathBase("/visiosys")`** no ASP.NET Core, lido de uma variável de configuração `PathBase` (vazia em desenvolvimento, preenchida em produção via `/etc/visiosys/production.env`). O middleware só reescreve requisições que já chegam com o prefixo; requisições sem prefixo passam intactas, então o acesso direto por IP (usado pelo health check do `deploy.yml`) continua funcionando sem nenhuma mudança no workflow de CI.
- **`base: '/visiosys/'`** no build do Vite (só no modo `build`; `npm run dev` continua servindo na raiz, sem afetar o fluxo de desenvolvimento local) e **`basename={import.meta.env.BASE_URL}`** no `BrowserRouter`, garantindo que os assets e as rotas client-side resolvam corretamente sob o path em produção.
- **nginx com dois server blocks independentes:** o bloco existente (`server_name _;`, acesso por IP, HTTP) fica intocado; um novo bloco (`server_name felipedearaujo.dev www.felipedearaujo.dev;`) cuida exclusivamente do domínio, com `/` redirecionando para `/visiosys/` e `/visiosys/` em proxy para o backend.
- **TLS via Let's Encrypt/certbot**, rodado com o plugin `certbot --nginx`, que edita automaticamente só o server block do domínio para adicionar HTTPS e o redirect 80→443 — o bloco do IP nunca ganha HTTPS, propositalmente, já que ele é só uma via de acesso operacional/CI.
- **Zona Route 53 reaproveitada:** como o domínio foi registrado direto no Route 53, a AWS já havia criado automaticamente uma hosted zone pública para ele no momento do registro. Em vez de criar uma segunda zone via Terraform (o que geraria uma zone órfã, já que a delegação de nameservers no registrador aponta para a zone original), a zone existente foi importada para o state (`terraform import aws_route53_zone.main[0] <ZONE_ID>`) antes do `apply`.

---

## Alternativas Consideradas

| Alternativa | Motivo de descarte |
|-------------|-------------------|
| **Subdomínio `visiosys.felipedearaujo.dev`** | Funcionaria igualmente bem, mas o autor prefere reservar a estrutura de subdomínios para outros usos futuros e manter o Visiosys como um path sob o domínio raiz |
| **CloudFront + ACM na frente do nginx** | Resolveria TLS de forma mais "gerenciada", mas adiciona uma camada de infraestrutura (distribuição, cache, invalidação) desproporcional para uma instância única de estudos; certbot é mais simples de operar e entender |
| **Criar uma nova hosted zone via Terraform sem importar a existente** | Geraria uma zone duplicada e órfã (a delegação de nameservers do registrador já aponta para a zone criada automaticamente no registro do domínio); exigiria um passo manual extra de redelegação sem necessidade |
| **Path strip no nginx (`proxy_pass http://localhost:5000/;` com barra final) em vez de `UsePathBase`** | Removeria o prefixo `/visiosys` antes de chegar ao backend, mas quebraria os links absolutos gerados pelo Swagger UI e exigiria reescrever manualmente toda referência a path absoluto no frontend; `UsePathBase` é a forma nativa do ASP.NET Core de lidar com isso, mantendo a aplicação ciente do seu próprio prefixo |

---

## Consequências

**Positivas:**
- Acesso público em HTTPS real, fechando a pendência da Fase 5.
- Resolve a preocupação de segurança de divulgar um link de demonstração em HTTP puro com login.
- Acesso por IP bruto (usado pelo `deploy.yml`) continua funcionando sem nenhuma alteração, já que `UsePathBase` é não destrutivo para requisições sem o prefixo.
- `user_data.sh` atualizado para refletir o estado final, então uma eventual recriação da instância nasce com domínio, path base e certbot já configurados (o Elastic IP é reassociado automaticamente, então o DNS já resolve para a nova instância no boot).

**Negativas / Trade-offs:**
- A URL pública do sistema fica acoplada a um domínio pessoal do autor (`felipedearaujo.dev`), não a um domínio dedicado ao projeto — aceitável no contexto de projeto de estudos/portfólio.
- O certificado Let's Encrypt expira em 90 dias; a renovação automática do certbot (`systemd timer`) cobre isso, mas depende da instância continuar no ar para renovar.
- Dois server blocks nginx para manter sincronizados manualmente caso a topologia de rede mude no futuro (ex: adoção de um Application Load Balancer).
