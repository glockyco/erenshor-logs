<script lang="ts">
  import { createQrSvgString } from "@svelte-put/qr";
  import { cn } from "$lib/utils";

  interface Props {
    data: string;
    size?: number;
    class?: string;
    alt?: string;
  }

  let { data, size = 200, class: className, alt = "QR Code" }: Props = $props();

  const svgString = $derived(
    createQrSvgString({
      data,
      width: size,
      height: size,
    })
  );
</script>

<div
  class={cn("inline-flex items-center justify-center rounded-lg bg-white p-4", className)}
  role="img"
  aria-label={alt}
>
  <!-- eslint-disable-next-line svelte/no-at-html-tags -->
  {@html svgString}
</div>
