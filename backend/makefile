.PHONY: build up down logs clean clean-all

install-linux:
	sudo apt-get update
	sudo apt-get install podman
	sudo apt-get install podman-compose

install-mac:
	brew install podman
	brew install podman-compose

init-mac:
	podman machine init
	podman machine start

build:
	podman-compose build

up:
	podman-compose up -d

down:
	podman-compose down

logs:
	podman-compose logs -f api
	podman-compose logs -f db

clean:
	podman-compose down -v
	podman system prune -f

clean-all: clean
	podman stop -a
	podman rm -af
	podman rmi -af
	podman volume prune -f
	podman volume rm -af
	podman network prune -f
