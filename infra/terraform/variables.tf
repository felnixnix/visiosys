variable "aws_region" {
  description = "Região AWS onde os recursos serão criados."
  type        = string
  default     = "sa-east-1"
}

variable "environment" {
  description = "Nome do ambiente (prod, staging)."
  type        = string
  default     = "prod"
}

variable "ec2_instance_type" {
  description = "Tipo da instância EC2 (ARM)."
  type        = string
  default     = "t4g.medium"
}

variable "ec2_key_pair_name" {
  description = "Nome do Key Pair cadastrado na AWS para acesso SSH."
  type        = string
}

variable "allowed_ssh_cidr" {
  description = "CIDR permitido para SSH (ex: '203.0.113.0/32'). Nunca usar '0.0.0.0/0'."
  type        = string
}

variable "rds_instance_class" {
  description = "Classe da instância RDS PostgreSQL."
  type        = string
  default     = "db.t4g.micro"
}

variable "rds_username" {
  description = "Usuário master do RDS PostgreSQL."
  type        = string
  default     = "visiosys_prod"
  sensitive   = true
}

variable "rds_password" {
  description = "Senha do usuário master do RDS PostgreSQL (min 16 chars)."
  type        = string
  sensitive   = true
}

variable "domain_name" {
  description = "Domínio da aplicação (ex: 'visiosys.com.br'). Vazio = Route 53 não é criado."
  type        = string
  default     = ""
}
