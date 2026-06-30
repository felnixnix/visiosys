output "ec2_public_ip" {
  description = "IP público (Elastic IP) da instância EC2."
  value       = aws_eip.app.public_ip
}

output "ec2_instance_id" {
  description = "ID da instância EC2."
  value       = aws_instance.app.id
}

output "rds_endpoint" {
  description = "Endpoint do RDS PostgreSQL (host:port)."
  value       = aws_db_instance.postgres.endpoint
}

output "rds_connection_string" {
  description = "Connection string (sem senha) para referência."
  value       = "Host=${aws_db_instance.postgres.address};Port=5432;Database=visiosys;Username=${var.rds_username};Password=<SENHA>"
  sensitive   = true
}

output "s3_bucket_name" {
  description = "Nome do bucket S3 de documentos."
  value       = aws_s3_bucket.documentos.bucket
}

output "nameservers" {
  description = "Name servers do Route 53 (copiar para o registrador do domínio)."
  value       = var.domain_name != "" ? aws_route53_zone.main[0].name_servers : []
}
