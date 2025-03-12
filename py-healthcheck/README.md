# py-healthcheck

This module can be executed with python directly using `python -m ldn_healthcheck` or by running the installable script `ldn_healthcheck`.

## Configuration

This app can be configured using the following environment variables:

| Name                | Description                                                                             |
|:--------------------|-----------------------------------------------------------------------------------------|
| `DEBUG`             | This can be set to change the logging mode to DEBUG. Otherwise it will default to INFO. |
| `LDN_SERVICE`       | The name of the systemd service to restart, in case the checks fail.                    |
| `DC_WEBHOOK`        | The URL of the discord webhook to notify.                                               |
| `DC_ROLEID`         | The role to notify.                                                                     |

## Checks

Currently, the following checks will be performed:

1. Is a TCP client able to connect to the server?
2. Is a LDN client able to initialize a session?
3. Is a LDN client able to host games on the server?

## Build

```commandline
poetry build
```