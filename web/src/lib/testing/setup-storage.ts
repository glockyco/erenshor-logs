import { beforeEach } from "vitest";
import { installStorageMock } from "./storage-mock";

installStorageMock();

beforeEach(() => {
  installStorageMock();
});
