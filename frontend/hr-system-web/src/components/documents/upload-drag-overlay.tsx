"use client";

import { useEffect, useState } from "react";

interface Props {
  onFileDrop: (file: File) => void;
}

export function UploadDragOverlay({ onFileDrop }: Props) {
  const [active, setActive] = useState(false);

  useEffect(() => {
    let depth = 0;

    function isFileDrag(e: DragEvent): boolean {
      return e.dataTransfer?.types?.includes("Files") ?? false;
    }

    function handleEnter(e: DragEvent) {
      if (!isFileDrag(e)) return;
      depth++;
      setActive(true);
    }
    function handleLeave(e: DragEvent) {
      if (!isFileDrag(e)) return;
      depth--;
      if (depth <= 0) {
        depth = 0;
        setActive(false);
      }
    }
    function handleOver(e: DragEvent) {
      if (!isFileDrag(e)) return;
      e.preventDefault();
    }
    function handleDrop(e: DragEvent) {
      if (!isFileDrag(e)) return;
      e.preventDefault();
      depth = 0;
      setActive(false);
      const file = e.dataTransfer?.files?.[0];
      if (file) onFileDrop(file);
    }

    window.addEventListener("dragenter", handleEnter);
    window.addEventListener("dragleave", handleLeave);
    window.addEventListener("dragover", handleOver);
    window.addEventListener("drop", handleDrop);
    return () => {
      window.removeEventListener("dragenter", handleEnter);
      window.removeEventListener("dragleave", handleLeave);
      window.removeEventListener("dragover", handleOver);
      window.removeEventListener("drop", handleDrop);
    };
  }, [onFileDrop]);

  if (!active) return null;
  return (
    <div className="pointer-events-none fixed inset-4 z-[60] flex flex-col items-center justify-center gap-2 rounded-2xl border-4 border-dashed border-primary bg-primary/10">
      <div className="text-3xl text-primary">↑</div>
      <div className="text-base font-semibold text-primary">Drop file to upload</div>
      <div className="text-xs text-foreground/70">Release to open the upload form</div>
    </div>
  );
}
