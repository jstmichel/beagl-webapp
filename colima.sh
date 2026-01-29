#!/bin/sh
# colima.sh - Start or stop Colima with Beagl resource limits and profile
# Usage: ./colima.sh start|stop

PROFILE="beagl-dev"
CPU="2"
MEMORY="2"

if [ "$1" = "start" ]; then
	colima start --cpus "$CPU" --memory "$MEMORY" --profile "$PROFILE"
elif [ "$1" = "stop" ]; then
	colima stop --profile "$PROFILE"
else
	echo "Usage: $0 start|stop"
	exit 1
fi
