resource "aws_security_group" "ec2" {
  name        = "visiosys-ec2-${var.environment}"
  description = "Visiosys EC2 - API, Worker, MongoDB, nginx"
  vpc_id      = aws_vpc.main.id

  ingress {
    description = "SSH restrito ao IP do operador"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [var.allowed_ssh_cidr]
  }

  ingress {
    description = "HTTP publico (redirect para HTTPS via nginx)"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS publico"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "visiosys-sg-ec2-${var.environment}" }
}

resource "aws_security_group" "rds" {
  name        = "visiosys-rds-${var.environment}"
  description = "Visiosys RDS PostgreSQL - acesso apenas da EC2"
  vpc_id      = aws_vpc.main.id

  ingress {
    description     = "PostgreSQL apenas da EC2"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ec2.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "visiosys-sg-rds-${var.environment}" }
}
