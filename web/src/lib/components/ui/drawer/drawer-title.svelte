<script lang="ts">
  import { Drawer as DrawerPrimitive } from "vaul-svelte";
  import { cn } from "$lib/utils.js";

  let { class: className, ...restProps }: DrawerPrimitive.TitleProps = $props();
</script>

<DrawerPrimitive.Title
  class={cn("text-sm font-semibold uppercase tracking-wider text-amber-500", className)}
  {...restProps}
/>
