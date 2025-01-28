COMPOSE_FILE = docker-compose.yaml
DC = docker compose -f $(COMPOSE_FILE)
SERVICES := $(shell awk '/^[ ]{2}[a-zA-Z0-9_-]+:$/ {print $$1}' $(COMPOSE_FILE))

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
	$(DC) down --remove-orphans

.PHONY: restart
restart: down up

.PHONY: logs
logs:
	@echo "Viewing logs of all services..."
	$(DC) logs -f

.PHONY: clean
clean:
	@echo "Cleaning docker containers, images, networks and volumes..."
	$(DC) down --rmi all --volumes --remove-orphans
	docker system prune -f

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

