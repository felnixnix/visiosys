resource "aws_db_instance" "postgres" {
  identifier        = "visiosys-${var.environment}"
  engine            = "postgres"
  engine_version    = "16"
  instance_class    = var.rds_instance_class
  allocated_storage = 20
  storage_type      = "gp3"
  storage_encrypted = true

  db_name  = "visiosys"
  username = var.rds_username
  password = var.rds_password

  db_subnet_group_name   = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.rds.id]

  # MVP: single-AZ para custo. Habilitar multi_az=true antes de go-live em produção crítica.
  multi_az               = false
  publicly_accessible    = false
  skip_final_snapshot    = false
  final_snapshot_identifier = "visiosys-${var.environment}-final"
  deletion_protection    = true

  backup_retention_period = 7
  backup_window           = "03:00-04:00"
  maintenance_window      = "Sun:04:00-Sun:05:00"

  tags = { Name = "visiosys-rds-${var.environment}" }
}
