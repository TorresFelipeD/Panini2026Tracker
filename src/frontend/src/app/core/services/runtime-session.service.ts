import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, OnDestroy } from '@angular/core';
import { APP_CONFIG, AppConfig } from '../tokens/app-config.token';

@Injectable({ providedIn: 'root' })
export class RuntimeSessionService implements OnDestroy {
  private readonly closeHandler: () => void;
  private readonly heartbeatIntervalId: number | null;
  private readonly sessionUrl: URL | null;

  constructor(
    private readonly http: HttpClient,
    @Inject(APP_CONFIG) private readonly config: AppConfig
  ) {
    const sessionToken = new URL(window.location.href).searchParams.get('desktopSession');
    if (!sessionToken) {
      this.sessionUrl = null;
      this.closeHandler = () => undefined;
      this.heartbeatIntervalId = null;
      return;
    }

    const apiBaseUrl = new URL(this.config.apiBaseUrl, window.location.origin);
    this.sessionUrl = new URL('runtime/session/', apiBaseUrl);
    this.closeHandler = () => {
      const closeUrl = new URL('closed', this.sessionUrl!);
      closeUrl.searchParams.set('token', sessionToken);
      navigator.sendBeacon(closeUrl.toString(), new Blob());
    };

    this.sendHeartbeat(sessionToken);
    this.heartbeatIntervalId = window.setInterval(() => {
      this.sendHeartbeat(sessionToken);
    }, 5000);

    window.addEventListener('beforeunload', this.closeHandler);
    window.addEventListener('pagehide', this.closeHandler);
  }

  ngOnDestroy(): void {
    if (this.heartbeatIntervalId !== null) {
      window.clearInterval(this.heartbeatIntervalId);
    }

    if (this.sessionUrl) {
      window.removeEventListener('beforeunload', this.closeHandler);
      window.removeEventListener('pagehide', this.closeHandler);
    }
  }

  private sendHeartbeat(sessionToken: string): void {
    if (!this.sessionUrl) {
      return;
    }

    const heartbeatUrl = new URL('heartbeat', this.sessionUrl);
    heartbeatUrl.searchParams.set('token', sessionToken);
    this.http.post(heartbeatUrl.toString(), {}).subscribe({
      error: () => undefined
    });
  }
}
