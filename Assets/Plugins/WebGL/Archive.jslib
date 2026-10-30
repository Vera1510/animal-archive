mergeInto(LibraryManager.library, {
  ArchiveState: function (json) { window.archiveState = JSON.parse(UTF8ToString(json)); }
});
