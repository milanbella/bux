(async () => {
  try {
    db = await connect('mongodb://admin_multi:J05g5gjywKKMEYIbEZDN@176.223.128.102:27018/gaos_multi?authSource=admin');
    console.log("INFO: Dropping mongo database 'gaos_multi'");
    await db.dropDatabase();
  } catch (error) {
    console.error("ERROR: Could not drop the database 'gaos_multi':", error.message);
    throw new Error("could not drop database 'gaos_multi'");
  }
})();
