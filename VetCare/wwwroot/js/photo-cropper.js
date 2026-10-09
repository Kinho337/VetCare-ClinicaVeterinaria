(() => {
  document.querySelectorAll("[data-photo-cropper]").forEach((root) => {
    const input = root.querySelector(".photo-input");
    const ratioSelect = root.querySelector(".crop-ratio");
    const canvas = root.querySelector(".crop-preview");
    const error = root.querySelector("[data-photo-error]");
    const ctx = canvas.getContext("2d");
    let image = null;
    let fileName = "foto.jpg";

    function draw() {
      if (!image) return;
      const ratio = Number(ratioSelect.value || 1);
      const boxW = 360, boxH = 240;
      let cropW = image.width, cropH = cropW / ratio;
      if (cropH > image.height) { cropH = image.height; cropW = cropH * ratio; }
      const sx = (image.width - cropW) / 2;
      const sy = (image.height - cropH) / 2;
      canvas.width = Math.max(1, Math.round(boxW));
      canvas.height = Math.max(1, Math.round(boxW / ratio));
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.drawImage(image, sx, sy, cropW, cropH, 0, 0, canvas.width, canvas.height);
    }

    input.addEventListener("change", () => {
      error.textContent = "";
      const file = input.files && input.files[0];
      if (!file) { image = null; return; }
      if (!file.type.startsWith("image/")) { error.textContent = "Selecione um arquivo de imagem."; input.value = ""; return; }
      if (file.size > 5 * 1024 * 1024) { error.textContent = "A imagem deve ter no máximo 5 MB."; input.value = ""; return; }
      fileName = file.name.replace(/\.[^.]+$/, "") + "-recortada.jpg";
      const reader = new FileReader();
      reader.onload = () => {
        image = new Image();
        image.onload = draw;
        image.onerror = () => { error.textContent = "Não foi possível abrir esta imagem."; };
        image.src = reader.result;
      };
      reader.readAsDataURL(file);
    });
    ratioSelect.addEventListener("change", draw);

    const form = root.closest("form");
    if (form) {
      form.addEventListener("submit", async (event) => {
        if (!image || !input.files || !input.files.length) return;
        event.preventDefault();
        draw();
        canvas.toBlob((blob) => {
          if (!blob) { error.textContent = "Não foi possível recortar a imagem."; return; }
          const cropped = new File([blob], fileName, { type: "image/jpeg" });
          const dt = new DataTransfer();
          dt.items.add(cropped);
          input.files = dt.files;
          form.submit();
        }, "image/jpeg", 0.9);
      }, { once: true });
    }
  });
})();
