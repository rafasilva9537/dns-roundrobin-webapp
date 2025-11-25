# dns-roundrobin-webapp
Multi-server ASP.NET web app load-balanced via DNS RR, where users can authenticate with JWT and access their profile on any node without losing login state.

# Setup
## Debian based distributions
- **Deactivate systemd-resolve**
  - Avoid conflicts with bind9 container listening on the same port.
  - On terminal, run command to edit file:
  - ```bash
    sudo nano /etc/systemd/resolved.conf
    ```
  - Remove comment from line `#DNSStubListener=yes` and change value to `no`: `DNSStubListener=no`.
  - If you want docker as default dns, set `DNS=127.0.0.1`.
    - For subnet access only, map docker ports to host private ip
    - If only the port is specified, all interfaces become open (0.0.0.0:53).
  - ```bash
    sudo systemctl restart systemd-resolved
    ```