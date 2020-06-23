using System;

public abstract class DownloaderBase : IDisposable
{
    protected DownloadState downloadState;
    protected ServerConfiguration serverConfiguration;
    protected DownloadPresenter downloadPresenter;
    
    public virtual void Initialize(DownloadState downloadState, ServerConfiguration serverConfiguration,
        DownloadPresenter downloadPresenter)
    {
        this.downloadState = downloadState;
        this.serverConfiguration = serverConfiguration;
        this.downloadPresenter = downloadPresenter;
    }

    public virtual void Dispose()
    {
        downloadState = null;
        serverConfiguration = null;
        downloadPresenter = null;
    }
}