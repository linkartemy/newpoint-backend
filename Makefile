COMPOSE_FILE = docker-compose.yml
DC = docker compose -f $(COMPOSE_FILE)
SERVICES := $(shell grep -E '^\s{2}[a-zA-Z0-9_-]+:' $(COMPOSE_FILE) | awk '{print $$1}' | tr -d ':')
DATABASE_MIGRATIONS_SERVICE := database-migrations

.PHONY: all
all: build up

.PHONY: build
build:
	@echo "Build all services..."
	$(DC) build

.PHONY: up
up:
	@echo "Starting all services..."
	$(DC) up -d

.PHONY: up-attach
up-attach:
	@echo "Starting all services with logs..."
	$(DC) up

.PHONY: down
down:
	@echo "Stopping and deleting all services..."
	$(DC) down

.PHONY: restart
restart: down up

.PHONY: restart-attach
restart-attach: down up-attach

.PHONY: full-restart-attach
full-restart-attach: down build up-attach

.PHONY: logs
logs:
	@echo "Viewing logs of all services..."
	$(DC) logs -f

.PHONY: build-%
build-%:
	@echo "Building service $*..."
	$(DC) build $*

.PHONY: up-%
up-%:
	@echo "Starting service $*..."
	$(DC) up -d $*

.PHONY: down-%
down-%:
	@echo "Stopping service $*..."
	$(DC) stop $*

.PHONY: restart-%
restart-%: down-% up-%

.PHONY: logs-%
logs-%:
	@echo "Viewing logs of service $*..."
	$(DC) logs -f $*

.PHONY: clean-%
clean-%:
	@echo "Cleaning service $*..."
	$(DC) rm -s -f $*

.PHONY: services
services:
	@echo "Available services:"
	@echo $(SERVICES) | tr ' ' '\n'

.PHONY: build-migrations
build-migrations:
	@echo "Building migrations service..."
	docker build -t database-migrations -f DatabaseMigrations/Dockerfile .

.PHONY: migrate
migrate: build-migrations
	docker run --rm --name database-migrations-container database-migrations

.PHONY: clean-migrations
clean-migrations:
	docker rmi database-migrations || true
