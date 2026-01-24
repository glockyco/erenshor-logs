<script lang="ts">
  import { Drawer as DrawerPrimitive } from "vaul-svelte";
  import { cn } from "$lib/utils.js";

  let { class: className, ...restProps } = $props();
</script>

<DrawerPrimitive.Title class={cn("font-semibold text-stone-100", className)} {...restProps} />
